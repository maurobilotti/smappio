package ar.com.smappio;

import android.content.ClipData;
import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.support.v7.app.AppCompatActivity;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.BaseAdapter;
import android.widget.CheckBox;
import android.widget.ImageView;
import android.widget.ListView;
import android.widget.TextView;

import java.io.File;
import java.io.FileFilter;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

public class FileChooserActivity extends AppCompatActivity {

    private static final String PARENT_DIR = "..";

    private ArrayList<String> filenames = new ArrayList<>();
    private ArrayList<String> filepaths = new ArrayList<>();
    private File currentPath;
    private File currentFile;
    private File smappioFile;
    private String extension = ".wav";

    private FileListAdapter fileListAdapter;
    private ListView listViewFiles;

    private MenuItem deleteBtn;
    private MenuItem shareBtn;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_file_chooser);

        //Flecha de la toolbar para volver al activity anterior
        if (getSupportActionBar() != null) {
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);
            getSupportActionBar().setDisplayShowHomeEnabled(true);
        }

        smappioFile = new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM), "Smappio");
        currentPath = smappioFile;

        refresh(currentPath);
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.menu_file_chooser, menu);
        deleteBtn = (MenuItem) menu.findItem(R.id.action_delete);
        shareBtn = (MenuItem) menu.findItem(R.id.action_share);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        int id = item.getItemId();
        if (id == android.R.id.home) {
            finish();
        }
        return super.onOptionsItemSelected(item);
    }

    @Override
    public void onBackPressed() {
        if (!currentPath.getPath().equals(smappioFile.getPath())) {
            refresh(currentPath.getParentFile());
        } else {
            super.onBackPressed();
        }
    }

    private void refresh(File path) {

        this.currentPath = path;

        if (path.exists()) {

            File[] dirs = path.listFiles(new FileFilter() {
                @Override
                public boolean accept(File file) {
                    return (file.isDirectory() && file.canRead());
                }
            });
            File[] files = path.listFiles(new FileFilter() {
                @Override
                public boolean accept(File file) {
                    if (!file.isDirectory()) {
                        if (!file.canRead()) {
                            return false;
                        } else if (extension == null) {
                            return true;
                        } else {
                            return file.getName().toLowerCase().endsWith(extension);
                        }
                    } else {
                        return false;
                    }
                }
            });

            Arrays.sort(dirs);
            Arrays.sort(files);

            filenames.clear();
            filepaths.clear();
            if (!currentPath.getPath().equals(smappioFile.getPath())) {
                filenames.add(PARENT_DIR);
                filepaths.add(currentPath.getParentFile().getPath());
            }
            for (File dir : dirs) {
                filenames.add(dir.getName());
                filepaths.add(dir.getPath());
            }
            for (File file : files) {
                filenames.add(file.getName());
                filepaths.add(file.getPath());
            }

            TextView currentPathLbl = (TextView) findViewById(R.id.currentPathLbl);
            currentPathLbl.setText(currentPath.getPath());
            fileListAdapter = new FileListAdapter(this, filenames, filepaths);
            listViewFiles = (ListView) findViewById(R.id.list_view_files);
            listViewFiles.setAdapter(fileListAdapter);
            listViewFiles.setOnItemClickListener(onItemClickListener);
            listViewFiles.setOnItemLongClickListener(onItemLongClickListener);
            listViewFiles.setChoiceMode(ListView.CHOICE_MODE_SINGLE);
        }
    }

    private AdapterView.OnItemClickListener onItemClickListener = new AdapterView.OnItemClickListener() {
        @Override
        public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
            clearSelection();
            String fileChosen = (String) filenames.get(position);
            File chosenFile = getChosenFile(fileChosen);
            if (chosenFile.isDirectory()) {
                refresh(chosenFile);
            } else {
                fileSelected(chosenFile);
            }
        }
    };

    private AdapterView.OnItemLongClickListener onItemLongClickListener = new AdapterView.OnItemLongClickListener() {
        @Override
        public boolean onItemLongClick(AdapterView<?> parent, View view, int position, long id) {
            clearSelection();
            String fileChosen = (String) filenames.get(position);
            File chosenFile = getChosenFile(fileChosen);
            if(!fileChosen.equals(PARENT_DIR)) {
                CheckBox checkBox = (CheckBox) view.findViewById(R.id.lr_cbCheck);
                checkBox.setChecked(true);
                checkBox.setVisibility(View.VISIBLE);
                currentFile = chosenFile;
                deleteBtn.setVisible(true);
                if (!chosenFile.isDirectory()) {
                    shareBtn.setVisible(true);
                }
            }
            return true;
        }
    };

    private void clearSelection() {
        deleteBtn.setVisible(false);
        shareBtn.setVisible(false);
        currentFile = null;
        for (int i = 0; i < listViewFiles.getCount(); i++) {
            CheckBox checkBox = (CheckBox)  listViewFiles.getChildAt(i).findViewById(R.id.lr_cbCheck);
            checkBox.setVisibility(View.INVISIBLE);
            checkBox.setChecked(false);
        }
    }

    private File getChosenFile(String fileChosen) {
        if (fileChosen.equals(PARENT_DIR)) {
            return currentPath.getParentFile();
        } else {
            return new File(currentPath, fileChosen);
        }
    }

    private void fileSelected(File chosenFile) {
        Uri currentFileURI = Uri.fromFile(chosenFile);
        Intent intent = new Intent(this, AudioPlayerActivity.class);
        intent.putExtra("currentFileURI", currentFileURI);
        startActivity(intent);
    }

    public void shareFile(MenuItem view) {
        Intent intent = new Intent(Intent.ACTION_SEND);
        intent.putExtra(Intent.EXTRA_STREAM, Uri.fromFile(currentFile));
        intent.setType("audio/*");
        startActivity(intent);
    }

    public void deleteFile(MenuItem item) {
    }

    // Adaptador
    class FileListAdapter extends BaseAdapter {

        private List<String> m_item;
        private List<String> m_path;
        Context m_context;

        public FileListAdapter(Context p_context, List<String> p_item, List<String> p_path) {
            m_context = p_context;
            m_item = p_item;
            m_path = p_path;
        }

        @Override
        public int getCount() {
            return m_item.size();
        }

        @Override
        public Object getItem(int position) {
            return m_item.get(position);
        }

        @Override
        public long getItemId(int position) {
            return position;
        }

        @Override
        public View getView(final int p_position, View p_convertView, ViewGroup p_parent) {
            View m_view = null;
            FileListAdapter.ViewHolder m_viewHolder = null;
            if (p_convertView == null) {
                LayoutInflater m_inflater = LayoutInflater.from(m_context);
                m_view = m_inflater.inflate(R.layout.element_file, null);
                m_viewHolder = new FileListAdapter.ViewHolder();
                m_viewHolder.m_tvFileName = (TextView) m_view.findViewById(R.id.lr_tvFileName);
                m_viewHolder.m_ivIcon = (ImageView) m_view.findViewById(R.id.lr_ivFileIcon);
                m_viewHolder.m_cbCheck = (CheckBox) m_view.findViewById(R.id.lr_cbCheck);
                m_view.setTag(m_viewHolder);
            } else {
                m_view = p_convertView;
                m_viewHolder = ((FileListAdapter.ViewHolder) m_view.getTag());
            }

            m_viewHolder.m_tvFileName.setText(m_item.get(p_position));
            m_viewHolder.m_ivIcon.setImageResource(setFileImageType(new File(m_path.get(p_position))));
//            m_viewHolder.m_cbCheck.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
//                @Override
//                public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
//                    if (isChecked) {
//                        m_selectedItem.add(p_position);
//                    } else {
//                        m_selectedItem.remove(m_selectedItem.indexOf(p_position));
//                    }
//                }
//            });

            return m_view;
        }

        class ViewHolder {
            ImageView m_ivIcon;
            TextView m_tvFileName;
            CheckBox m_cbCheck;
        }

        private int setFileImageType(File m_file) {
            if (m_file.isDirectory())
                return R.drawable.ic_folder;
            else {
                return R.drawable.ic_heart;
            }
        }

        public Object getPath(int position) {
            return m_path.get(position);
        }
    }
}
