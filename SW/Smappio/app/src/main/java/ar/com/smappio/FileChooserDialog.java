package ar.com.smappio;

import android.app.ActionBar;
import android.app.Activity;
import android.app.Dialog;
import android.content.Context;
import android.content.DialogInterface;
import android.os.Environment;
import android.support.v7.app.AlertDialog;
import android.text.Editable;
import android.text.TextWatcher;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.BaseAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.ListView;
import android.widget.TextView;

import java.io.File;
import java.io.FileFilter;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

public class FileChooserDialog {

    private final Activity activity;
    private ListView list;
    private Dialog dialog;
    private Button createButton;

    private ArrayList<String> filenames = new ArrayList<>();
    private ArrayList<String> filepaths = new ArrayList<>();
    private File currentPath;
    private File smappioFile;

    private FileListAdapter adapter;

    public interface FileSelectedListener {
        void fileSelected(File file);
    }

    public FileChooserDialog setFileListener(FileSelectedListener fileListener) {
        this.fileListener = fileListener;
        return this;
    }

    private FileSelectedListener fileListener;

    public FileChooserDialog(Activity activity) {
        this.activity = activity;
        LayoutInflater layoutInflater = LayoutInflater.from(activity);
        View popupFileManager = layoutInflater.inflate(R.layout.popup_file_manager, null);
        list = (ListView) popupFileManager.findViewById(R.id.list_view_files2);
        createButton = (Button) popupFileManager.findViewById(R.id.button_create);
        createButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                crearCarpeta();
            }
        });
        dialog = new Dialog(activity);
        dialog.setContentView(popupFileManager);
        dialog.getWindow().setLayout(ActionBar.LayoutParams.MATCH_PARENT, ActionBar.LayoutParams.MATCH_PARENT);

        smappioFile = new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM), "Smappio");
        currentPath = smappioFile;

        refresh(currentPath);
    }

    public void showDialog() {
        dialog.show();
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

            Arrays.sort(dirs);

            filenames.clear();
            filepaths.clear();

            for (File dir : dirs) {
                filenames.add(dir.getName());
                filepaths.add(dir.getPath());
            }

            adapter = new FileListAdapter(activity, filenames, filepaths);
            list.setAdapter(adapter);
            list.setOnItemClickListener(onItemClickListener);

        }
    }

    private AdapterView.OnItemClickListener onItemClickListener = new AdapterView.OnItemClickListener() {
        @Override
        public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
            String fileChosen = (String) list.getItemAtPosition(position);
            File chosenFile = getChosenFile(fileChosen);
            if (fileListener != null) {
                fileListener.fileSelected(chosenFile);
            }
            dialog.dismiss();
        }
    };

    private File getChosenFile(String fileChosen) {
        return new File(currentPath, fileChosen);
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
                m_view.setTag(m_viewHolder);
            } else {
                m_view = p_convertView;
                m_viewHolder = ((FileListAdapter.ViewHolder) m_view.getTag());
            }

            m_viewHolder.m_tvFileName.setText(m_item.get(p_position));
            m_viewHolder.m_ivIcon.setImageResource(setFileImageType(new File(m_path.get(p_position))));

            return m_view;
        }

        class ViewHolder {
            ImageView m_ivIcon;
            TextView m_tvFileName;
        }

        private int setFileImageType(File m_file) {
            return R.drawable.ic_folder;
        }

        public Object getPath(int position) {
            return m_path.get(position);
        }
    }

    public void crearCarpeta() {

        dialog.dismiss();
        LayoutInflater layoutInflater = LayoutInflater.from(activity);
        View popupSaveFileView = layoutInflater.inflate(R.layout.popup_save_file, null);
        EditText userInput = (EditText) popupSaveFileView.findViewById(R.id.file_name);;
        AlertDialog.Builder builder = new AlertDialog.Builder(activity);
        builder.setView(popupSaveFileView);
        AlertDialog alertDialog = builder.setTitle("Crear carpeta")
                .setMessage("Ingrese el nombre de la carpeta.")
                .setCancelable(true)
                .setPositiveButton("Aceptar", new DialogInterface.OnClickListener() {
                    public void onClick(final DialogInterface dialog, final int id) {
                        if (userInput.getText() != null && !userInput.getText().toString().isEmpty()) {
                            String nameFile = userInput.getText().toString();
                            String filePath = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM).toString() + File.separator + "Smappio" + File.separator + nameFile;
                            try {
                                File file = new File(filePath);
                                if (!file.exists()) {
                                    Log.d("FOLDER", "Folder doesn't exist, creating it...");
                                    boolean rv = file.mkdir();
                                    Log.d("FOLDER", "Folder creation " + ( rv ? "success" : "failed"));
                                    if (fileListener != null) {
                                        fileListener.fileSelected(file);
                                    }
                                    dialog.dismiss();
                                } else {
                                    Log.d("FOLDER", "Folder already exists.");
                                }
                            } catch (Exception e) {
                                e.printStackTrace();
                            }
                        }
                    }
                })
                .setNegativeButton("Cancelar", new DialogInterface.OnClickListener() {
                    public void onClick(final DialogInterface dialog, final int id) {
                        dialog.cancel();
                    }
                })
                .create();

        userInput.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {
            }
            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
            }
            @Override
            public void afterTextChanged(Editable s) {
                if (userInput.getText() != null && !userInput.getText().toString().isEmpty()){
                    alertDialog.getButton(AlertDialog.BUTTON_POSITIVE).setEnabled(true);
                } else {
                    alertDialog.getButton(AlertDialog.BUTTON_POSITIVE).setEnabled(false);
                }
            }
        });

        alertDialog.show();
        alertDialog.getButton(AlertDialog.BUTTON_POSITIVE).setEnabled(false);
    }

}
